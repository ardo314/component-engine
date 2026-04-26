import type { Pose } from "@ardo314/core";
import { followPoseComponent } from "@ardo314/in-memory";
import { type EntityId, entityIdSchema } from "@engine/core";
import { ComponentWorker, Implements } from "@engine/worker";

@Implements(followPoseComponent)
export class FollowPoseWorker extends ComponentWorker {
  private _target: EntityId = entityIdSchema.parse("");
  private _pose: Pose = [0, 0, 0, 0, 0, 0];

  getTarget() {
    return this._target;
  }

  setTarget(input: EntityId) {
    this._target = input;
  }

  getPose() {
    return this._pose;
  }

  setPose(input: Pose) {
    this._pose = input;
  }
}
