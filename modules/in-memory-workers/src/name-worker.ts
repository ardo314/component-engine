import { name } from "@ardo314/in-memory";
import { ComponentProperty, ComponentWorker } from "@engine/module";

export class NameWorker extends ComponentWorker<typeof name> {
  readonly value: ComponentProperty<string>;
}
