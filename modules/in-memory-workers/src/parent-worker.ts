import { parent } from "@ardo314/in-memory";
import { EntityId, entityIdSchema } from "@engine/core";
import { ComponentProperty, ComponentWorker } from "@engine/module";

export class ParentWorker extends ComponentWorker<typeof parent> {
  private _value: EntityId = entityIdSchema.parse("");

  readonly value: ComponentProperty<EntityId>;

  constructor() {
    super();

    const self = this;

    this.value = {
      async get() {
        return self._value;
      },
      async set(value: EntityId) {
        self._value = value;
      },
    };
  }
}
