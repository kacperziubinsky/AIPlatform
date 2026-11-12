export type CvParseStatus = "Pending" | "Processing" | "Completed" | "Failed";

export interface UserCV {
  id: string;
  filename: string;
  uploadDate: string;
  parseStatus: CvParseStatus | string;
}

export function parseUserCV(data: Record<string, unknown>): UserCV {
  return {
    id: String(data.id ?? ""),
    filename: String(data.fileName ?? ""),
    uploadDate: String(data.uploadedAt ?? ""),
    parseStatus: String(data.parseStatus ?? ""),
  };
}
