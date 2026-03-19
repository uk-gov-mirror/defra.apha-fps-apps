-- View: fps.qvrd_splitmonthly

CREATE OR REPLACE VIEW fps.qvrd_splitmonthly AS
 SELECT month,
    fpsyear,
    sum(ltsplitfee) AS totalltsplitfee
   FROM fps.vpostmort_vrd_split
  GROUP BY month, fpsyear;
