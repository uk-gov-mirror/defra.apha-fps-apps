-- View: fps.vvrd_split

CREATE OR REPLACE VIEW fps.vvrd_split AS
 SELECT qvrd_splitlocationmonthly.location,
    qvrd_splitlocationmonthly.fpsyear,
    sum((qvrd_splitlocationmonthly.labltsplitfee / qvrd_splitmonthly.totalltsplitfee)) AS split
   FROM (fps.qvrd_splitmonthly
     JOIN fps.qvrd_splitlocationmonthly ON (((qvrd_splitmonthly.month = qvrd_splitlocationmonthly.month) AND (qvrd_splitmonthly.fpsyear = qvrd_splitlocationmonthly.fpsyear))))
  GROUP BY qvrd_splitlocationmonthly.location, qvrd_splitlocationmonthly.fpsyear
  ORDER BY qvrd_splitlocationmonthly.location;
