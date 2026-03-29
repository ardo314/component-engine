import { pose } from "@ardo314/in-memory";
import { defineComponentWorker } from "@engine/module";

export const poseWorker = defineComponentWorker(pose, () => {
  let position: [number, number, number] = [0, 0, 0];
  let rotation: [number, number, number] = [0, 0, 0];

  return {
    position: {
      async get() {
        return position;
      },
      async set(v) {
        position = v;
      },
    },
    rotation: {
      async get() {
        return rotation;
      },
      async set(v) {
        rotation = v;
      },
    },
  };
});
