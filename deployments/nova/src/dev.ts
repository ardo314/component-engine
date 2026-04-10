import { execSync } from "node:child_process";
import { fileURLToPath } from "node:url";
import { dirname, resolve } from "node:path";
import {
  type App,
  ApplicationApi,
  Configuration,
} from "@wandelbots/nova-api/v2";
import { isAxiosError } from "axios";
import { backendApp, editorApp } from "./apps.js";

// ---------------------------------------------------------------------------
// CLI flags
// ---------------------------------------------------------------------------
const args = process.argv.slice(2);
const skipBuild = args.includes("--skip-build");
const backendOnly = args.includes("--backend-only");
const editorOnly = args.includes("--editor-only");

const includeBackend = !editorOnly;
const includeEditor = !backendOnly;

// ---------------------------------------------------------------------------
// Environment
// ---------------------------------------------------------------------------
const novaApi = process.env.NOVA_API;
const cellName = process.env.CELL_NAME;
const natsBroker = process.env.NATS_BROKER;

if (!novaApi) throw new Error("NOVA_API is not set");
if (!cellName) throw new Error("CELL_NAME is not set");
if (!natsBroker) throw new Error("NATS_BROKER is not set");

const NOVA_API_URL = novaApi;
const CELL = cellName;
const NATS = natsBroker;

const BACKEND_IMAGE = "ghcr.io/ardo314/component-engine-backend:dev";
const EDITOR_IMAGE = "ghcr.io/ardo314/component-engine-editor:dev";

const repoRoot = resolve(
  dirname(fileURLToPath(import.meta.url)),
  "../../../..",
);

function run(cmd: string, cwd = repoRoot) {
  console.log(`\n> ${cmd}`);
  execSync(cmd, { cwd, stdio: "inherit" });
}

// ---------------------------------------------------------------------------
// Build
// ---------------------------------------------------------------------------
if (!skipBuild) {
  console.log("\n=== Building TypeScript ===");
  run("npm run build");

  if (includeBackend) {
    console.log("\n=== Building backend Docker image ===");
    run(`docker build -f engine/backend/Dockerfile -t ${BACKEND_IMAGE} .`);
    console.log("\n=== Pushing backend image ===");
    run(`docker push ${BACKEND_IMAGE}`);
  }

  if (includeEditor) {
    console.log("\n=== Building editor Docker image ===");
    run(`docker build -f engine/editor/Dockerfile -t ${EDITOR_IMAGE} .`);
    console.log("\n=== Pushing editor image ===");
    run(`docker push ${EDITOR_IMAGE}`);
  }
}

// ---------------------------------------------------------------------------
// Deploy
// ---------------------------------------------------------------------------
const config = new Configuration({ basePath: `${NOVA_API_URL}/api/v2` });
const api = new ApplicationApi(config);

async function deleteApp(name: string) {
  console.log(`Deleting app '${name}' from cell '${CELL}'...`);
  try {
    await api.deleteApp(CELL, name);
    console.log(`  -> '${name}' deleted`);
  } catch (err) {
    if (isAxiosError(err) && err.response?.status === 404) {
      console.log(`  -> '${name}' not found, skipping`);
    } else {
      throw err;
    }
  }
}

async function addApp(app: App) {
  console.log(`Installing app '${app.name}' into cell '${CELL}'...`);
  await api.addApp(CELL, app);
  console.log(`  -> '${app.name}' installed`);
}

console.log("\n=== Deploying to NOVA ===");

if (includeBackend) {
  await deleteApp("component-engine-backend");
  await addApp(backendApp(BACKEND_IMAGE, NATS, CELL));
}

if (includeEditor) {
  await deleteApp("component-engine-editor");
  await addApp(editorApp(EDITOR_IMAGE, "/nats", CELL));
}

console.log("\nDev deployment complete.");
