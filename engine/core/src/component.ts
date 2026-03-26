type Id = string & { readonly __brand: unique symbol };

type ComponentContract = {
  readonly properties: Record<string, any>;
};

interface Component {
  readonly id: Id;
  readonly contract: ComponentContract;
}

export function defineComponent<const C extends ComponentContract>(
  id: Id,
  contract: C,
): { readonly id: Id; readonly contract: C } {
  return {
    id: id,
    contract: contract,
  };
}

const exampleComponent = defineComponent("example" as Id, {
  properties: {
    name: "ExampleComponent",
    value: 42,
  },
});

export type ComponentProxy<T extends Component> = T["contract"]["properties"];
type ExampleComponent = ComponentProxy<typeof exampleComponent>;
