#!/usr/bin/env node
import {
  requireCommand,
  requireDockerDaemon,
  seedDatabase,
  startPostgres,
  waitForPostgres,
} from "./_lib.mjs";

async function main() {
  console.log("==> Checking required tools");
  await requireDockerDaemon();
  await requireCommand("dotnet");

  console.log("==> Ensuring PostgreSQL is running");
  await startPostgres();
  await waitForPostgres();

  console.log("==> Seeding development data");
  await seedDatabase();
}

main().catch((error) => {
  console.error(error.message ?? error);
  process.exit(1);
});
