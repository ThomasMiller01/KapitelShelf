import TuneIcon from "@mui/icons-material/Tune";
import { IconButtonWithTooltip } from "../../../components/base/IconButtonWithTooltip";

export const Settings: React.FC = () => {
  return (
    <>
      <IconButtonWithTooltip tooltip="Settings">
        <TuneIcon />
      </IconButtonWithTooltip>
    </>
  );
};
