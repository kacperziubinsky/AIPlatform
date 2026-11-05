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
