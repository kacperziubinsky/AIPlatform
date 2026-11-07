export interface CreateApplication{
    offerId: string,
    cvId: string,
    coverLetter: string,
}

export interface Application{
    id: string,
    offerId: string,
    status: string,
    createdAt: string,
}