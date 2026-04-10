import type { App } from "@wandelbots/nova-api/v2";

export function backendApp(
  image: string,
  natsBroker: string,
  cellName: string,
): App {
  return {
    name: "component-engine-backend",
    app_icon: "favicon.ico",
    container_image: { image },
    port: 8080,
    environment: [
      { name: "NATS_URL", value: natsBroker },
      { name: "BASE_PATH", value: `/${cellName}/component-engine-backend` },
    ],
  };
}

export function editorApp(
  image: string,
  natsUrl: string,
  cellName: string,
): App {
  return {
    name: "component-engine-editor",
    app_icon: "favicon.ico",
    container_image: { image },
    port: 8080,
    environment: [
      { name: "NATS_URL", value: natsUrl },
      { name: "BASE_PATH", value: `/${cellName}/component-engine-editor` },
    ],
  };
}
