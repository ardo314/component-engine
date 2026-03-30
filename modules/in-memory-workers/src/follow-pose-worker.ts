import { followPoseComponent } from "@ardo314/in-memory";
import { EntityId, entityIdSchema } from "@engine/core";
import { defineComponentWorker } from "@engine/module";

export const followPoseWorker = defineComponentWorker(
  followPoseComponent,
  () => {
    let _target: EntityId = entityIdSchema.parse("");

    const target = {
      async get() {
        return _target;
      },
      async set(v: EntityId) {
        _target = v;
      },
    };

    return {
      target,
    };
  },
);
