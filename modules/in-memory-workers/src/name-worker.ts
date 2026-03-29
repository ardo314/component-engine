import { name } from "@ardo314/in-memory";
import { defineComponentWorker } from "@engine/module";

export const nameWorker = defineComponentWorker(name, () => {
  let _value = "";

  const value = {
    async get() {
      return _value;
    },
    async set(v: string) {
      _value = v;
    },
  };

  return {
    value,
  };
});
