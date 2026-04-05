import { defineComponent, entityIdSchema } from "@engine/core";
import {
  poseComponent as corePoseComponent,
  nameComponent as coreNameComponent,
  parentComponent as coreParentComponent,
} from "@ardo314/core";

export const nameComponent = defineComponent("nova.name", {
  composites: [coreNameComponent],
});

export const parentComponent = defineComponent("nova.parent", {
  composites: [coreParentComponent],
});

export const poseComponent = defineComponent("nova.pose", {
  composites: [corePoseComponent],
});

export const followPoseComponent = defineComponent("nova.follow-pose", {
  properties: {
    target: entityIdSchema,
  },
});
