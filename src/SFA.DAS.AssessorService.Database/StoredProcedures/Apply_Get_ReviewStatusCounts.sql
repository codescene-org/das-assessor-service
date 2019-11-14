﻿
CREATE PROCEDURE [dbo].[Apply_Get_ReviewStatusCounts]
	@openSequenceStatus AS NVARCHAR(MAX),
	@feedbackSequenceStatus AS NVARCHAR(MAX),
	@closedSequenceStatus AS NVARCHAR(MAX),
	@excludedApplicationStatus AS NVARCHAR(MAX),
	@excludedReviewStatus AS NVARCHAR(MAX)
AS
BEGIN
	SELECT ReviewStatus, COUNT(*) Total
	FROM
	(
		SELECT * FROM [dbo].[Apply_Func_Get_Applications] (1, null, @openSequenceStatus, @excludedApplicationStatus, @excludedReviewStatus, null)
		UNION
		SELECT * FROM [dbo].[Apply_Func_Get_Applications] (1, null, @feedbackSequenceStatus, @excludedApplicationStatus, @excludedReviewStatus, null)
		UNION
		SELECT * FROM [dbo].[Apply_Func_Get_Applications] (1, null, @closedSequenceStatus, @excludedApplicationStatus, @excludedReviewStatus, null)
	) [OrganisationApplications]
	GROUP BY ReviewStatus

	SELECT ReviewStatus, COUNT(*) Total
	FROM
	(
		SELECT * FROM [dbo].[Apply_Func_Get_Applications] (2, null, @openSequenceStatus, @excludedApplicationStatus, @excludedReviewStatus, null)
		UNION
		SELECT * FROM [dbo].[Apply_Func_Get_Applications] (2, null, @feedbackSequenceStatus, @excludedApplicationStatus, @excludedReviewStatus, null)
		UNION
		SELECT * FROM [dbo].[Apply_Func_Get_Applications] (2, null, @closedSequenceStatus, @excludedApplicationStatus, @excludedReviewStatus, null)
	) [StandardApplications]
	GROUP BY ReviewStatus
END
