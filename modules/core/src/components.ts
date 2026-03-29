import { defineComponent, entityIdSchema } from "@engine/core";
import { z } from "zod";

export const name = defineComponent("core.name", {
  properties: {
    value: z.string(),
  },
});

export const parent = defineComponent("core.parent", {
  properties: {
    value: entityIdSchema,
  },
});
