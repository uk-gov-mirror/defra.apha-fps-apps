-- View: fps.qvrd_splitlocationmonthly

CREATE OR REPLACE VIEW fps.qvrd_splitlocationmonthly AS
 SELECT location,
    month,
    fpsyear,
    sum(ltsplitfee) AS labltsplitfee
   FROM fps.vpostmort_vrd_split
  GROUP BY location, month, fpsyear
  ORDER BY location, month;
