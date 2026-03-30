import { Pose } from "@ardo314/core";
import { poseComponent } from "@ardo314/in-memory";
import { defineComponentWorker } from "@engine/module";

export const poseWorker = defineComponentWorker(poseComponent, () => {
  let _value: Pose = [0, 0, 0, 0, 0, 0];

  return {
    value: {
      async get() {
        return _value;
      },
      async set(v: Pose) {
        _value = v;
      },
    },
  };
});
