-- View: fps.vvrd_splitlocation

CREATE OR REPLACE VIEW fps.vvrd_splitlocation AS
 SELECT qvrd_splitlocationmonthly.location,
    qvrd_splitlocationmonthly.fpsyear,
    sum((qvrd_splitlocationmonthly.labltsplitfee / qvrd_splitmonthly.totalltsplitfee)) AS splitmultiplier
   FROM (fps.qvrd_splitlocationmonthly
     JOIN fps.qvrd_splitmonthly ON (((qvrd_splitlocationmonthly.month = qvrd_splitmonthly.month) AND (qvrd_splitlocationmonthly.fpsyear = qvrd_splitmonthly.fpsyear))))
  GROUP BY qvrd_splitlocationmonthly.location, qvrd_splitlocationmonthly.fpsyear;
