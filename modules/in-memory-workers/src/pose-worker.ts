import { pose } from "@ardo314/in-memory";
import { ComponentProperty, ComponentWorker } from "@engine/module";

export class PoseWorker extends ComponentWorker<typeof pose> {
  readonly value: ComponentProperty<string>;
}
