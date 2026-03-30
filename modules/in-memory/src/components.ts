import { defineSchema, defineComponent, entityIdSchema } from "@engine/core";
import { poseSchema } from "@ardo314/core";
import { z } from "zod";

export const nameSchema = defineSchema("in-memory.name", {
  properties: {
    value: z.string(),
  },
});

export const parentSchema = defineSchema("in-memory.parent", {
  properties: {
    value: entityIdSchema,
  },
});

export const posePropertySchema = defineSchema("in-memory.pose", {
  properties: {
    value: poseSchema,
  },
});

export const followPoseSchema = defineSchema("in-memory.follow-pose", {
  properties: {
    target: entityIdSchema,
  },
});

export const nameComponent = defineComponent(nameSchema);
export const parentComponent = defineComponent(parentSchema);
export const poseComponent = defineComponent(posePropertySchema);
export const followPoseComponent = defineComponent(followPoseSchema);
