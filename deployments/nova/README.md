# @engine/nova-deploy

NOVA cell app installer and dev deployment tooling for component-engine. Uses `@wandelbots/nova-api` to manage apps via the NOVA API.

## Entry Points

### `install-apps` — Production

Runs inside a NOVA cell app container. Installs the backend and editor as cell apps, then stays alive.

NOVA auto-injects the required environment variables:

| Variable       | Description                              |
| -------------- | ---------------------------------------- |
| `NOVA_API`     | API endpoint reachable from the container |
| `CELL_NAME`    | Name of the hosting cell                 |
| `NATS_BROKER`  | NATS broker endpoint                     |

Optional overrides:

| Variable        | Default                                                |
| --------------- | ------------------------------------------------------ |
| `VERSION`       | `latest`                                               |
| `BACKEND_IMAGE` | `ghcr.io/ardo314/component-engine-backend:${VERSION}`  |
| `EDITOR_IMAGE`  | `ghcr.io/ardo314/component-engine-editor:${VERSION}`   |

### `dev` — Development

Runs locally. Builds Docker images with `:dev` tags, pushes them, then deletes and reinstalls the apps in a NOVA cell.

#### Prerequisites

- Docker logged in to `ghcr.io` (`docker login ghcr.io`)
- Environment variables set: `NOVA_API`, `CELL_NAME`, `NATS_BROKER`

#### Usage

```bash
# Full cycle: build → push → deploy
NOVA_API=https://... CELL_NAME=my-cell NATS_BROKER=nats://... \
  npm run dev --workspace=@engine/nova-deploy

# Skip build (just redeploy with existing :dev images)
npm run dev --workspace=@engine/nova-deploy -- --skip-build

# Deploy backend only
npm run dev --workspace=@engine/nova-deploy -- --backend-only

# Deploy editor only
npm run dev --workspace=@engine/nova-deploy -- --editor-only
```

#### Flags

| Flag              | Effect                                          |
| ----------------- | ----------------------------------------------- |
| `--skip-build`    | Skip TypeScript and Docker builds, just redeploy |
| `--backend-only`  | Only build and deploy the backend                |
| `--editor-only`   | Only build and deploy the editor                 |
