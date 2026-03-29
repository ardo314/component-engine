import { pose } from "@ardo314/in-memory";
import { defineComponentWorker } from "@engine/module";

export const poseWorker = defineComponentWorker(pose, () => {
  let _value: [number, number, number, number, number, number] = [
    0, 0, 0, 0, 0, 0,
  ];

  return {
    value: {
      async get() {
        return _value;
      },
      async set(v: [number, number, number, number, number, number]) {
        _value = v;
      },
    },
  };
});
