import DeleteIcon from "@mui/icons-material/Delete";
import EditSquareIcon from "@mui/icons-material/EditSquare";
import MoreVertIcon from "@mui/icons-material/MoreVert";
import SaveIcon from "@mui/icons-material/Save";
import StarBorderIcon from "@mui/icons-material/StarBorder";
import {
  Button,
  Card,
  CardContent,
  ListItemIcon,
  ListItemText,
  Menu,
  MenuItem,
  Rating,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import { useState } from "react";
import { useParams } from "react-router-dom";
import { ButtonWithTooltip } from "../../components/base/ButtonWithTooltip";
import { IconButtonWithTooltip } from "../../components/base/IconButtonWithTooltip";
import { useUserProfile } from "../../hooks/useUserProfile";
import { UserBookMetadataDTO } from "../../lib/api/KapitelShelf.Api";
import { useAddOrUpdateBookRating } from "../../lib/requests/users/useAddOrUpdateBookRating";
import { useDeleteBookRating } from "../../lib/requests/users/useDeleteBookRating";
import { FormatTimeUntil } from "../../utils/TimeUtils";

interface UserBookMetadataProps {
  userMetadata?: UserBookMetadataDTO;
  isCurrentUser: boolean;
}

export const EditableUserBookMetadata: React.FC<UserBookMetadataProps> = ({
  userMetadata,
  isCurrentUser,
}) => {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const { profile } = useUserProfile();

  const { mutateAsync: addOrUpdateBookRating } = useAddOrUpdateBookRating({
    bookId,
    userId: profile?.id,
  });

  const [isinEditMode, setIsInEditMode] = useState(false);

  const onSave = (rating: number | null, notes: string) => {
    addOrUpdateBookRating({
      rating: rating === 0 ? null : rating,
      notes: notes === "" ? null : notes,
    }).then(() => {
      setIsInEditMode(false);
    });
  };

  if (!userMetadata && isCurrentUser && !isinEditMode) {
    return (
      <Button
        sx={{ width: "fit-content" }}
        startIcon={<StarBorderIcon />}
        // endIcon={<ExpandMoreIcon />}
        onClick={() => setIsInEditMode(true)}
        variant="outlined"
        color="secondary"
      >
        Add Rating
      </Button>
    );
  }

  return (
    <Card
      sx={{
        border: isCurrentUser ? "1px solid" : "none",
        borderColor: "secondary.main",
      }}
    >
      <CardContent
        sx={{
          pt: "15px !important",
          pb: "15px !important",
        }}
      >
        {/* Display normal user book metadata */}
        {userMetadata && !isinEditMode && (
          <UserBookMetadataContent
            userMetadata={userMetadata}
            isCurrentUser={isCurrentUser}
            onEnterEditMode={() => setIsInEditMode(true)}
          />
        )}

        {/* Display editable user book metadata */}
        {isCurrentUser && isinEditMode && (
          <EditableUserBookMetadataContent
            userMetadata={userMetadata}
            onSave={onSave}
          />
        )}
      </CardContent>
    </Card>
  );
};

interface UserBookMetadataContentProps {
  userMetadata: UserBookMetadataDTO;
  isCurrentUser: boolean;
  onEnterEditMode: () => void;
}

const UserBookMetadataContent: React.FC<UserBookMetadataContentProps> = ({
  userMetadata,
  isCurrentUser,
  onEnterEditMode,
}) => {
  return (
    <>
      <Stack
        direction="row"
        justifyContent="space-between"
        alignItems="center"
        mb={userMetadata.notes ? 1.5 : 0}
      >
        <Stack direction="row" spacing={2} alignItems="center">
          <Typography
            gutterBottom
            sx={{ color: "text.secondary", fontSize: 14 }}
          >
            {userMetadata.user?.username}
          </Typography>
          {userMetadata.rating && (
            <Rating
              value={userMetadata.rating / 2}
              max={5}
              precision={0.5}
              readOnly
              size="small"
            />
          )}
        </Stack>
        {isCurrentUser && (
          <IconButtonWithTooltip
            tooltip="Edit"
            size="small"
            onClick={onEnterEditMode}
            color="secondary"
          >
            <EditSquareIcon fontSize="small" />
          </IconButtonWithTooltip>
        )}
      </Stack>
      {userMetadata.notes && (
        <Typography variant="body2" sx={{ whiteSpace: "pre-line" }}>
          {userMetadata.notes}
        </Typography>
      )}
      <Typography sx={{ color: "text.secondary", mt: 1, fontSize: 12 }}>
        {FormatTimeUntil(userMetadata.createdOn, true)}
      </Typography>
    </>
  );
};

interface EditableUserBookMetadataContentProps {
  userMetadata: UserBookMetadataDTO | undefined;
  onSave: (rating: number | null, notes: string) => void;
}

const EditableUserBookMetadataContent: React.FC<
  EditableUserBookMetadataContentProps
> = ({ userMetadata, onSave }) => {
  const { profile } = useUserProfile();

  const [rating, setRating] = useState<number | undefined | null>(
    (userMetadata?.rating ?? 0) / 2,
  );
  const [notes, setNotes] = useState(userMetadata?.notes ?? "");

  return (
    <>
      <Stack
        direction={{ xs: "column", md: "row" }}
        justifyContent="space-between"
        alignItems={{ xs: "start", md: "center" }}
        mb={1.5}
      >
        <Stack direction="row" spacing={2} alignItems="center">
          <Typography
            gutterBottom
            sx={{ color: "text.secondary", fontSize: 14 }}
          >
            {profile?.username}
          </Typography>
          <Rating
            value={rating}
            onChange={(_, value) => setRating(value)}
            max={5}
            precision={0.5}
            size="small"
          />
        </Stack>
        <Stack
          direction="row"
          spacing={{ xs: 1, md: 2 }}
          alignItems="center"
          justifyContent="end"
          sx={{
            width: {
              xs: "100%",
              md: "fit-content",
            },
          }}
        >
          {/* Save Button */}
          <ButtonWithTooltip
            tooltip="Save"
            size="small"
            startIcon={<SaveIcon />}
            onClick={() => onSave(rating ? rating * 2 : null, notes)}
            disabled={(rating == null || rating === 0) && notes === ""}
          >
            Save
          </ButtonWithTooltip>

          {/* Options */}
          {userMetadata && <OptionsMenu />}
        </Stack>
      </Stack>
      <TextField
        label="Your Comment"
        multiline
        rows={3}
        variant="filled"
        fullWidth
        value={notes}
        onChange={(e) => setNotes(e.target.value)}
      />
    </>
  );
};

const OptionsMenu = () => {
  const { bookId } = useParams<{
    bookId: string;
  }>();
  const { profile } = useUserProfile();

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>): void => {
    setAnchorEl(event.currentTarget);
  };
  const handleClose = (): void => {
    setAnchorEl(null);
  };

  const { mutateAsync: deleteBookRating } = useDeleteBookRating({
    bookId,
    userId: profile?.id,
  });

  return (
    <>
      <IconButtonWithTooltip
        tooltip="More options"
        onClick={handleClick}
        size="small"
        color="primary"
      >
        <MoreVertIcon fontSize="small" />
      </IconButtonWithTooltip>
      <Menu anchorEl={anchorEl} open={open} onClose={handleClose}>
        {/* Delete */}
        <MenuItem onClick={() => deleteBookRating()}>
          <ListItemIcon>
            <DeleteIcon fontSize="small" color="primary" />
          </ListItemIcon>
          <ListItemText>Delete</ListItemText>
        </MenuItem>
      </Menu>
    </>
  );
};
