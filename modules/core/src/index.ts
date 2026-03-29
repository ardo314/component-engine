import { z } from "zod";

export const vector2Schema = z.tuple([z.number(), z.number()]);

export type Vector2 = z.infer<typeof vector2Schema>;

export const vector3Schema = z.tuple([z.number(), z.number(), z.number()]);

export type Vector3 = z.infer<typeof vector3Schema>;

export const rotationVectorSchema = z.tuple([
  z.number(),
  z.number(),
  z.number(),
]);

export type RotationVector = z.infer<typeof rotationVectorSchema>;

export const quaternionSchema = z.tuple([
  z.number(),
  z.number(),
  z.number(),
  z.number(),
]);

export type Quaternion = z.infer<typeof quaternionSchema>;

export const poseSchema = z.tuple([
  z.number(),
  z.number(),
  z.number(),
  z.number(),
  z.number(),
  z.number(),
]);

export type Pose = z.infer<typeof poseSchema>;
