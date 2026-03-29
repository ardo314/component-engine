import { pose } from "@ardo314/in-memory";
import { ComponentProperty, ComponentWorker } from "@engine/module";

export class PoseWorker extends ComponentWorker<typeof pose> {
  private _value: string = "";

  readonly value: ComponentProperty<string>;

  constructor() {
    super();

    const self = this;

    this.value = {
      async get() {
        return self._value;
      },
      async set(value: string) {
        self._value = value;
      },
    };
  }
}
