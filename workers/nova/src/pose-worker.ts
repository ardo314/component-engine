import type { Pose } from "@ardo314/core";
import { poseComponent } from "@ardo314/nova";
import { ComponentWorker, Implements } from "@engine/worker";

@Implements(poseComponent)
export class PoseWorker extends ComponentWorker {
  private _pose: Pose = [0, 0, 0, 0, 0, 0];

  getPose() {
    return this._pose;
  }

  setPose(input: Pose) {
    this._pose = input;
  }
}
