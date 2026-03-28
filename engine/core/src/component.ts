import { z } from "zod";

type ComponentId = string & { readonly __brand: unique symbol };

type MethodSchema = {
  readonly input?: z.ZodType;
  readonly output?: z.ZodType;
};

type ComponentSchema = {
  readonly properties?: Record<string, z.ZodType>;
  readonly methods?: Record<string, MethodSchema>;
};

export interface Component {
  readonly id: ComponentId;
  readonly contract: ComponentSchema;
}

export function defineComponent<const C extends ComponentSchema>(
  id: ComponentId,
  contract: C,
): { readonly id: ComponentId; readonly contract: C } {
  return {
    id: id,
    contract: contract,
  };
}

type InferProperties<P extends Record<string, z.ZodType>> = {
  [K in keyof P]: {
    get(): Promise<z.infer<P[K]>>;
    set(value: z.infer<P[K]>): Promise<void>;
  };
};

type InferMethod<M extends MethodSchema> = M["input"] extends z.ZodType
  ? M["output"] extends z.ZodType
    ? (input: z.infer<M["input"]>) => Promise<z.infer<M["output"]>>
    : (input: z.infer<M["input"]>) => Promise<void>
  : M["output"] extends z.ZodType
    ? () => Promise<z.infer<M["output"]>>
    : () => Promise<void>;

type InferMethods<M extends Record<string, MethodSchema>> = {
  [K in keyof M]: InferMethod<M[K]>;
};

export type ComponentProxy<T extends Component> =
  (T["contract"]["properties"] extends Record<string, z.ZodType>
    ? InferProperties<T["contract"]["properties"]>
    : unknown) &
    (T["contract"]["methods"] extends Record<string, MethodSchema>
      ? InferMethods<T["contract"]["methods"]>
      : unknown);
