import { defineComponent, entityIdSchema } from "@engine/core";
import { poseSchema } from "@ardo314/core";
import { z } from "zod";

export const name = defineComponent("in-memory.name", {
  properties: {
    value: z.string(),
  },
});

export const parent = defineComponent("in-memory.parent", {
  properties: {
    value: entityIdSchema,
  },
});

export const pose = defineComponent("in-memory.pose", {
  properties: {
    value: poseSchema,
  },
});
