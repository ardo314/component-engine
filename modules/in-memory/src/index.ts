import { defineComponent, defineMethod, entityIdSchema } from "@engine/core";
import {
  getPose,
  setPose,
  getName,
  setName,
  getParent,
  setParent,
} from "@ardo314/core";

export const nameComponent = defineComponent("in-memory.name", [
  getName,
  setName,
]);

export const parentComponent = defineComponent("in-memory.parent", [
  getParent,
  setParent,
]);

export const poseComponent = defineComponent("in-memory.pose", [
  getPose,
  setPose,
]);

export const getTarget = defineMethod("getTarget", {
  output: entityIdSchema,
});

export const setTarget = defineMethod("setTarget", {
  input: entityIdSchema,
});

export const followPoseComponent = defineComponent("in-memory.follow-pose", [
  getTarget,
  setTarget,
  getPose,
  setPose,
]);
