import { FinishedReasonNullable } from "../lib/api/KapitelShelf.Api/api";

export const TaskCategoryColors: Record<string, string> = {
  Maintenance: "#A13B00",
  ["Cloud Storage"]: "#005662",
  Watchlist: "#53286e",
  default: "rgba(255, 255, 255, 0.16)",
};

const IsSpecialTaskCategoryColor = (category: string): boolean =>
  Object.keys(TaskCategoryColors).includes(category);

export const GetTaskCategoryColor = (category: string | undefined): string =>
  IsSpecialTaskCategoryColor(category ?? "")
    ? TaskCategoryColors[category ?? ""]
    : TaskCategoryColors.default;

export const GetTaskFinishedReasonString = (
  reason: FinishedReasonNullable
): string => {
  switch (reason) {
    case FinishedReasonNullable.NUMBER_0:
      return "Completed";
    case FinishedReasonNullable.NUMBER_1:
      return "Error";
    default:
      return GetTaskFinishedReasonString(FinishedReasonNullable.NUMBER_0);
  }
};
