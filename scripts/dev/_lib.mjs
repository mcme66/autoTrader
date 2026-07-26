import { execFileSync, spawn } from "node:child_process";
import { existsSync, copyFileSync } from "node:fs";
import { dirname, join } from "node:path";
import { fileURLToPath } from "node:url";

export const rootDir = join(dirname(fileURLToPath(import.meta.url)), "..", "..");
export const composeFile = "docker-compose.dev.yml";
export const apiProject = "backend/src/FinanceAnalysis.Api/FinanceAnalysis.Api.csproj";
export const solution = "backend/FinanceAnalysis.slnx";

const isWin = process.platform === "win32";

/** @type {Map<string, string>} */
const resolvedCommands = new Map();

/** @param {string} command */
function resolveCommand(command) {
  const cached = resolvedCommands.get(command);
  if (cached) {
    return cached;
  }

  if (!isWin || command.includes("\\") || command.includes("/")) {
    resolvedCommands.set(command, command);
    return command;
  }

  try {
    const output = execFileSync("where.exe", [command], { encoding: "utf8" });
    const first = output
      .split(/\r?\n/)
      .map((line) => line.trim())
      .find(Boolean);

    // Prefer .cmd/.bat shims for npm; .exe for everything else.
    const lines = output
      .split(/\r?\n/)
      .map((line) => line.trim())
      .filter(Boolean);
    const preferred =
      lines.find((line) => /\.cmd$/i.test(line) || /\.bat$/i.test(line))
      ?? lines.find((line) => /\.exe$/i.test(line))
      ?? first
      ?? command;

    resolvedCommands.set(command, preferred);
    return preferred;
  } catch {
    resolvedCommands.set(command, command);
    return command;
  }
}

/**
 * @param {string} command
 * @param {string[]} args
 * @param {{ cwd?: string, env?: NodeJS.ProcessEnv, allowFail?: boolean, silent?: boolean, timeoutMs?: number }} [options]
 */
export function run(command, args, options = {}) {
  const {
    cwd = rootDir,
    env = process.env,
    allowFail = false,
    silent = false,
    timeoutMs,
  } = options;
  const resolved = resolveCommand(command);

  return new Promise((resolve, reject) => {
    // .cmd/.bat shims (npm) require a shell on Windows; quote paths with spaces.
    const needsShell = isWin && /\.(cmd|bat)$/i.test(resolved);
    const file = needsShell ? `"${resolved}"` : resolved;
    const child = spawn(file, args, {
      cwd,
      env,
      stdio: silent ? "ignore" : "inherit",
      shell: needsShell,
    });

    /** @type {NodeJS.Timeout | undefined} */
    let timer;
    if (timeoutMs) {
      timer = setTimeout(() => {
        child.kill("SIGTERM");
        if (allowFail) {
          resolve(124);
          return;
        }

        reject(new Error(`${command} ${args.join(" ")} timed out after ${timeoutMs}ms`));
      }, timeoutMs);
    }

    child.on("error", (error) => {
      if (timer) {
        clearTimeout(timer);
      }

      reject(error);
    });
    child.on("exit", (code) => {
      if (timer) {
        clearTimeout(timer);
      }

      if (code === 0 || allowFail) {
        resolve(code ?? 0);
        return;
      }

      reject(new Error(`${command} ${args.join(" ")} exited with code ${code}`));
    });
  });
}

/** @param {string} name */
export async function requireCommand(name) {
  try {
    if (isWin) {
      execFileSync("where.exe", [name], { stdio: "ignore" });
    } else {
      execFileSync("which", [name], { stdio: "ignore" });
    }
  } catch {
    throw new Error(`Required command not found on PATH: ${name}`);
  }

  // Warm the resolver cache after we know the command exists.
  resolveCommand(name);
}

/**
 * Verifies the Docker daemon is reachable before compose commands. A common Windows failure
 * mode is a healthy CLI talking to a crashed Docker Desktop engine over the named pipe,
 * which surfaces as a confusing API 500 rather than "daemon not running".
 */
export async function requireDockerDaemon() {
  await requireCommand("docker");

  const code = await run("docker", ["info"], {
    allowFail: true,
    silent: true,
    timeoutMs: 15_000,
  });
  if (code === 0) {
    return;
  }

  throw new Error(
    [
      "Docker is installed, but the Docker Desktop engine is not responding.",
      "The CLI reached //./pipe/dockerDesktopLinuxEngine and got an error (often HTTP 500 / API version).",
      "",
      "Fix:",
      "  1. Open Docker Desktop and wait until it says Engine running.",
      "  2. If it stays unhealthy: Quit Docker Desktop, then start it again.",
      "  3. Still broken: Docker Desktop → Troubleshoot → Restart / Repair,",
      "     or restart the WSL backend: wsl --shutdown  (then reopen Docker Desktop).",
      "",
      "Confirm with: docker info",
      "Then re-run your npm script.",
    ].join("\n"),
  );
}

/**
 * @param {string} relativeSource
 * @param {string} relativeTarget
 */
export function ensureEnvCopy(relativeSource, relativeTarget) {
  const source = join(rootDir, relativeSource);
  const target = join(rootDir, relativeTarget);

  if (existsSync(target)) {
    console.log(`Keeping existing ${relativeTarget}`);
    return;
  }

  if (!existsSync(source)) {
    console.warn(`Skipped ${relativeTarget}: missing ${relativeSource}`);
    return;
  }

  copyFileSync(source, target);
  console.log(`Created ${relativeTarget} from ${relativeSource}`);
}

export async function startPostgres() {
  await run("docker", ["compose", "-f", composeFile, "up", "-d"]);
}

export async function wipePostgres() {
  await run("docker", ["compose", "-f", composeFile, "down", "-v", "--remove-orphans"]);
}

export async function waitForPostgres({ attempts = 40, delayMs = 1000 } = {}) {
  for (let i = 1; i <= attempts; i++) {
    const code = await run(
      "docker",
      [
        "compose",
        "-f",
        composeFile,
        "exec",
        "-T",
        "postgres",
        "pg_isready",
        "-U",
        "finance",
        "-d",
        "finance_analysis",
      ],
      { allowFail: true },
    );

    if (code === 0) {
      console.log("PostgreSQL is ready.");
      return;
    }

    process.stdout.write(`Waiting for PostgreSQL (${i}/${attempts})...\n`);
    await new Promise((r) => setTimeout(r, delayMs));
  }

  throw new Error("PostgreSQL did not become ready in time.");
}

/** Apply EF migrations (and exit) without starting the HTTP server. */
export async function migrateDatabase() {
  await run("dotnet", ["run", "--project", apiProject, "--", "--migrate"], {
    env: {
      ...process.env,
      ASPNETCORE_ENVIRONMENT: "Development",
    },
  });
}

/** Apply migrations, load demo data, then exit. */
export async function seedDatabase() {
  await run("dotnet", ["run", "--project", apiProject, "--", "--seed"], {
    env: {
      ...process.env,
      ASPNETCORE_ENVIRONMENT: "Development",
    },
  });
}
