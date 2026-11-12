export type ApplicationStatus =
  | "Submitted"
  | "InReview"
  | "Interview"
  | "Offered"
  | "Hired"
  | "Rejected"
  | "Withdrawn";

export type AiEvaluationStatus = "Pending" | "Processing" | "AwaitingAnswers" | "Completed" | "Failed";

export type MatchLevel = "None" | "Weak" | "Partial" | "Full";

export type EvaluationSource = "AI" | "Recruiter";

export interface CreateApplication {
  offerId: string;
  cvId: string;
  coverLetter?: string;
}

export interface CreatedApplication {
  id: string;
  offerId: string;
  status: ApplicationStatus;
  createdAt: string;
}

export interface MyApplication {
  id: string;
  offerId: string;
  offerTitle: string | null;
  company: string | null;
  status: ApplicationStatus;
  appliedAt: string;
  updatedAt: string;
}

export interface RequirementAssessment {
  requirementId: string;
  name: string;
  weight: number;
  isRequired: boolean;
  level: MatchLevel;
  evidence: string | null;
}

export interface Evaluation {
  id: string;
  score: number;
  comment: string | null;
  source: EvaluationSource;
  breakdown: RequirementAssessment[];
  createdAt: string;
}

export interface RecruiterApplication {
  id: string;
  offerId: string;
  applicantId: string;
  cvId: string;
  coverLetter: string | null;
  status: ApplicationStatus;
  aiStatus: AiEvaluationStatus;
  aiScore: number | null;
  aiError: string | null;
  evaluations: Evaluation[];
  createdAt: string;
}

export interface ChangeStatusRequest {
  status: ApplicationStatus;
  note?: string;
}

export interface CreateEvaluationRequest {
  score: number;
  comment?: string;
}

export const STATUS_LABELS: Record<ApplicationStatus, { label: string; className: string }> = {
  Submitted: { label: "Wysłana", className: "bg-blue-100 text-blue-800" },
  InReview: { label: "W trakcie", className: "bg-amber-100 text-amber-800" },
  Interview: { label: "Rozmowa", className: "bg-purple-100 text-purple-800" },
  Offered: { label: "Oferta", className: "bg-teal-100 text-teal-800" },
  Hired: { label: "Zatrudniony", className: "bg-lime-100 text-lime-800" },
  Rejected: { label: "Odrzucona", className: "bg-red-100 text-red-800" },
  Withdrawn: { label: "Wycofana", className: "bg-gray-100 text-gray-600" },
};

export const FINAL_STATUSES: ApplicationStatus[] = ["Hired", "Rejected", "Withdrawn"];

const PIPELINE: ApplicationStatus[] = ["Submitted", "InReview", "Interview", "Offered", "Hired"];

export function allowedNextStatuses(from: ApplicationStatus): ApplicationStatus[] {
  if (FINAL_STATUSES.includes(from)) return [];
  const index = PIPELINE.indexOf(from);
  return [...PIPELINE.slice(index + 1), "Rejected"];
}
