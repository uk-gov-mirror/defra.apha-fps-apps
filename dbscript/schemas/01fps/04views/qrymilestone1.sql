-- View: fps.qrymilestone1

CREATE OR REPLACE VIEW fps.qrymilestone1 AS
 SELECT DISTINCT project,
    milestoneref,
    plandate,
    actualdate,
    monthnofin AS duemonth,
        CASE
            WHEN (actualdate <= plandate) THEN (1)::numeric
            ELSE (0)::numeric
        END AS ontimeflag,
        CASE
            WHEN (actualdate IS NULL) THEN 0
            ELSE 1
        END AS completeflag,
    year,
    fpsyear
   FROM fps.milestone
  WHERE ((year)::text = '2003/2004'::text);
