#!/usr/bin/env node
import { join } from "node:path";
import {
  ensureEnvCopy,
  migrateDatabase,
  requireCommand,
  requireDockerDaemon,
  rootDir,
  run,
  solution,
  startPostgres,
  waitForPostgres,
} from "./_lib.mjs";

async function main() {
  console.log("==> Checking required tools");
  await requireCommand("node");
  await requireCommand("npm");
  await requireCommand("dotnet");
  await requireDockerDaemon();

  console.log("==> Ensuring local env files");
  ensureEnvCopy(".env.example", ".env");
  ensureEnvCopy("frontend/.env.example", "frontend/.env");

  console.log("==> Installing root npm dependencies");
  await run("npm", ["install"], { cwd: rootDir });

  console.log("==> Installing frontend npm dependencies");
  await run("npm", ["install"], { cwd: join(rootDir, "frontend") });

  console.log("==> Restoring .NET packages");
  await run("dotnet", ["restore", solution]);

  console.log("==> Starting PostgreSQL (docker compose)");
  await startPostgres();
  await waitForPostgres();

  console.log("==> Applying database migrations");
  await migrateDatabase();

  console.log("");
  console.log("Setup complete. Next steps:");
  console.log("  npm run seed   # optional demo user, portfolio, and sample prices");
  console.log("  npm run dev    # API + SPA together");
}

main().catch((error) => {
  console.error(error.message ?? error);
  process.exit(1);
});
