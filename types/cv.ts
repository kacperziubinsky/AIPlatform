export interface UserCV {
    id: string;
    filename: string;
    uploadDate: string;
}

export function parseUserCV(data: Record<string, unknown>): UserCV {
  return {
    id: String(data.id ?? data.cvId ?? ""),
    filename: String(
      data.filename ?? data.fileName ?? data.originalFilename ??
      data.originalFileName ?? data.name ?? ""
    ),
    uploadDate: String(
      data.uploadDate ?? data.uploadedAt ?? data.createdAt ??
      data.createdDate ?? data.dateAdded ?? ""
    ),
  };
}