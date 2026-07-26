#!/usr/bin/env node
import {
  migrateDatabase,
  requireCommand,
  requireDockerDaemon,
  startPostgres,
  waitForPostgres,
  wipePostgres,
} from "./_lib.mjs";

async function main() {
  console.log("==> Checking required tools");
  await requireDockerDaemon();
  await requireCommand("dotnet");

  console.log("==> Wiping PostgreSQL volume (all local data will be lost)");
  await wipePostgres();

  console.log("==> Starting a fresh PostgreSQL container");
  await startPostgres();
  await waitForPostgres();

  console.log("==> Re-applying database migrations");
  await migrateDatabase();

  console.log("");
  console.log("Database reset complete. Run `npm run seed` to reload demo data.");
}

main().catch((error) => {
  console.error(error.message ?? error);
  process.exit(1);
});
