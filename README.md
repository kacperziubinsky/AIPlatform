### 1. Auth Service

* `POST /auth/auth/register` — User registration (Applicant, Recruiter)
* `POST /auth/auth/login` — User authentication

### 2. User Service

* `GET /user/api/users/me` — Retrieve authenticated user profile
* `PUT /user/api/users/me` — Update authenticated user profile
* `GET /user/api/users` — List candidates (Recruiter only)
* `GET /user/api/users/{userId}` — Get candidate details by ID (Recruiter only)
* `POST /user/internal/users/{userId}` — Internal service route (Blocked by gateway / 404)

### 3. CV Service

* `POST /user/api/users/me/cvs` — Upload PDF resume (Applicant only)
* `GET /user/api/users/me/cvs` — List uploaded CVs with parsing status
* `GET /user/api/users/me/cvs/{cvId}` — Get CV details and parsed data
* `DELETE /user/api/users/me/cvs/{cvId}` — Delete CV record

### 4. Offer Service

* `POST /offer/offers` — Create job listing (Recruiter only)
* `GET /offer/offers` — Browse/Search job offers with query params (`searchTerm`, `category`, `minSalary`)
* `GET /offer/offers/{offerId}` — Get public job offer details
* `PUT /offer/offers/{offerId}` — Update job offer (Author Recruiter only)
* `DELETE /offer/offers/{offerId}` — Delete job offer (Author Recruiter only)

### 5. Recruitment & Applications

* `POST /recruitment/api/applications` — Submit application for an offer (Applicant only)
* `GET /recruitment/api/applications/me` — View applicant's own submitted applications
* `GET /recruitment/api/offers/{offerId}/applications` — View applications for a specific offer (Author Recruiter only)
* `GET /recruitment/api/applications/{applicationId}` — Get single application details with AI evaluation status
* `PATCH /recruitment/api/applications/{applicationId}/status` — Update application status (`Submitted`, `InReview`, `Interview`, `Hired`, `Rejected`, `Withdrawn`)
* `DELETE /recruitment/api/applications/{applicationId}` — Withdraw application (Applicant only)

### 6. Evaluations & AI

* `POST /recruitment/api/applications/{applicationId}/evaluations` — Add manual recruiter evaluation & score
* `POST /recruitment/api/applications/{applicationId}/ai-evaluation` — Trigger/re-run automated AI score evaluation
* 
[![Architecture diagram of kacperziubinsky/aiplatform](https://gitdiagram.com/kacperziubinsky/aiplatform/diagram.png)](https://gitdiagram.com/kacperziubinsky/aiplatform?utm_source=readme&utm_medium=picture)
