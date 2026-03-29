import { z } from "zod";

export const poseSchema = z.tuple([
  z.number(),
  z.number(),
  z.number(),
  z.number(),
  z.number(),
  z.number(),
]);

export type Pose = z.infer<typeof poseSchema>;
