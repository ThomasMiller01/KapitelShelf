import { useMutation } from "@tanstack/react-query";
import { useEffect } from "react";
import { useApi } from "../../contexts/ApiProvider";
import { useUserProfile } from "../useUserProfile";

export const useSessionStart = () => {
  const { profile } = useUserProfile();
  const { clients } = useApi();

  const { mutate: dispatchSessionStart } = useMutation({
    mutationFn: async (userId: string) =>
      await clients.hooks.hooksSessionStartPost(userId),
  });

  useEffect(() => {
    if (!profile?.id) {
      return;
    }

    dispatchSessionStart(profile.id);
  }, [profile?.id, dispatchSessionStart]);
};
