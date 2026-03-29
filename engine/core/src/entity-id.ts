import z from "zod";

/**
 * A serializable unique identifier for an entity.
 */
export type EntityId = string & { readonly __brand: unique symbol };

export const entityIdSchema = z.string() as unknown as z.ZodType<EntityId>;
