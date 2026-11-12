export interface Requirement {
  id: string;
  name: string;
  description: string;
  weight: number;
  isRequired: boolean;
}

export interface JobOffer {
  id: string;
  authorId: string;
  title: string;
  description: string;
  salary: number;
  category: number;
  stack: string[];
  experience: number;
  requirements: Requirement[];
  startDate: string;
  endDate: string;
  createdOn: string;
  company: string;
}


export interface OfferFilterRequest {
  searchTerm?: string;
  category?: Category;
  experience?: Experience;
  minSalary?: number;
  maxSalary?: number;
  limit?: number;
}

export enum Category {
  Ai = 0,
  Backend = 1,
  Pm = 2,
  FullStack = 3,
  Software = 4,
  Frontend = 5,
}

export enum Experience {
  Trainee = 0,
  Junior = 1,
  Middle = 2,
  Senior = 3,
}


export interface DictionaryItem {
  value: number;
  name: string;
  label: string;
}

export interface OfferDictionaries {
  categories: DictionaryItem[];
  experiences: DictionaryItem[];
}

export const CATEGORY_OPTIONS: DictionaryItem[] = [
  { value: Category.Ai, name: "Ai", label: "AI" },
  { value: Category.Backend, name: "Backend", label: "Backend" },
  { value: Category.Pm, name: "Pm", label: "Project Management" },
  { value: Category.FullStack, name: "FullStack", label: "Full Stack" },
  { value: Category.Software, name: "Software", label: "Software" },
  { value: Category.Frontend, name: "Frontend", label: "Frontend" },
];

export const EXPERIENCE_OPTIONS: DictionaryItem[] = [
  { value: Experience.Trainee, name: "Trainee", label: "Trainee" },
  { value: Experience.Junior, name: "Junior", label: "Junior" },
  { value: Experience.Middle, name: "Middle", label: "Mid" },
  { value: Experience.Senior, name: "Senior", label: "Senior" },
];

export const DEFAULT_OFFER_DICTIONARIES: OfferDictionaries = {
  categories: CATEGORY_OPTIONS,
  experiences: EXPERIENCE_OPTIONS,
};

export function dictionaryLabel(items: DictionaryItem[], value: number | null | undefined): string {
  return items.find((item) => item.value === value)?.label ?? "—";
}

export const OFFER_LIMITS = {
  title: 200,
  description: 5000,
  company: 200,
} as const;

export interface OfferRequirementRequest {
  name: string;
  description: string;
  weight: number;
  isRequired: boolean;
}
 
export interface CreateOfferRequest {
  title: string;
  description: string;
  salary: number;
  category: Category;
  experience: Experience;
  stack: string[];
  requirements: OfferRequirementRequest[];
  startDate: string;
  endDate: string;
  company: string;
}

export type UpdateOfferRequest = CreateOfferRequest;