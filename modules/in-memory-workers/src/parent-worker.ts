import { parent } from "@ardo314/in-memory";
import { ComponentProperty, ComponentWorker } from "@engine/module";

export class ParentWorker extends ComponentWorker<typeof parent> {
  readonly value: ComponentProperty<string>;
}
