import { defineComponent, entityIdSchema } from "@engine/core";
import { poseSchema } from "@ardo314/core";
import { z } from "zod";

export const nameComponent = defineComponent("in-memory.name", {
  properties: {
    value: z.string(),
  },
});

export const parentComponent = defineComponent("in-memory.parent", {
  properties: {
    value: entityIdSchema,
  },
});

export const poseComponent = defineComponent("in-memory.pose", {
  properties: {
    value: poseSchema,
  },
});

export const followPoseComponent = defineComponent("in-memory.follow-pose", {
  properties: {
    target: entityIdSchema,
  },
});
