export const TaskCategoryColors: Record<string, string> = {
  Maintenance: "#A13B00",
  Cloud: "#005662",
  default: "rgba(255, 255, 255, 0.16)",
};

const IsSpecialTaskCategoryColor = (category: string) =>
  Object.keys(TaskCategoryColors).includes(category);

export const GetTaskCategoryColor = (category: string | undefined): string =>
  IsSpecialTaskCategoryColor(category ?? "")
    ? TaskCategoryColors[category ?? ""]
    : TaskCategoryColors.default;
