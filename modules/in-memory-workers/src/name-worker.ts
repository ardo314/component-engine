import { nameComponent } from "@ardo314/in-memory";
import { defineComponentWorker } from "@engine/module";

export const nameWorker = defineComponentWorker(nameComponent, () => {
  let _value = "";

  return {
    value: {
      async get() {
        return _value;
      },
      async set(v: string) {
        _value = v;
      },
    },
  };
});
