import { followPose } from "@ardo314/in-memory";
import { EntityId, entityIdSchema } from "@engine/core";
import { defineComponentWorker } from "@engine/module";

export const followPoseWorker = defineComponentWorker(followPose, () => {
  let _target: EntityId = entityIdSchema.parse("");

  return {
    target: {
      async get() {
        return _target;
      },
      async set(v: EntityId) {
        _target = v;
      },
    },
  };
});
