import { ComponentProxy, defineComponent, EntityId } from "@engine/core";
import { entityIdSchema } from "@engine/core/src/entity-id.js";
import { z } from "zod";

export const name = defineComponent("nova.name", {
  properties: {
    value: z.string(),
  },
});

export type Name = typeof name;
export type NameProxy = ComponentProxy<Name>;

export const parent = defineComponent("nova.parent", {
  properties: {
    value: entityIdSchema,
  },
});

export type Parent = typeof parent;
export type ParentProxy = ComponentProxy<Parent>;

export const pose = defineComponent("nova.pose", {
  properties: {
    position: z.tuple([z.number(), z.number(), z.number()]),
    rotation: z.tuple([z.number(), z.number(), z.number()]),
  },
});

export type Pose = typeof pose;
export type PoseProxy = ComponentProxy<Pose>;
