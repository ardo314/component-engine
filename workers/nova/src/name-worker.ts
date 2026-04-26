import { nameComponent } from "@ardo314/nova";
import { ComponentWorker, Implements } from "@engine/worker";

@Implements(nameComponent)
export class NameWorker extends ComponentWorker {
  private _name = "";

  getName() {
    return this._name;
  }

  setName(input: string) {
    this._name = input;
  }
}
