import { defineComponent } from "@engine/core";
import { z } from "zod";

export const Name = defineComponent("nova.name", {
  properties: {
    value: z.string(),
  },
});

export const Parent = defineComponent("nova.parent", {
  properties: {
    value: z.string(),
  },
});
