-- View: fps.vqryfrmtimesellerpc

CREATE OR REPLACE VIEW fps.vqryfrmtimesellerpc AS
SELECT
    pc.conttarget,
    pcg.profitcentre                                     AS sellingpc,
    pcg.chargerate,
    pcg.ohr,
    bsum.sumofgenbid,
    wgg.workgroup,
    wgg.profitcentregrade,
    wgg.wggrade,
    ah.sumofplannedhours                                 AS apphours,
    sum(sjh.plannedhours)                                AS hrs,
    sum(we.hrsavail)                                     AS avhrs,
    (sum(sjh.plannedhours) * pcg.chargerate)             AS fec,
    (ah.sumofplannedhours * pcg.chargerate)              AS appfec,
    (pcg.ohr * sum(sjh.plannedhours))                    AS contribution,
    we.fpsyear,
    u.user_id,
    u.dt2username,
    u.useremail
FROM fps.tblkpprofitcentre pc
JOIN fps.tbluser_profitcentre upc ON pc.profitcentre = upc.profitcentre
JOIN fps.tblusers u               ON upc.user_id = u.user_id
JOIN fps.profitcentregrade pcg    ON pc.profitcentre = pcg.profitcentre
LEFT JOIN fps.vqrytbidsum bsum    ON pcg.profitcentre = bsum.profitcentre
                                 AND pcg.fpsyear      = bsum.fpsyear
                                 AND u.user_id         = bsum.user_id
JOIN fps.workgroupgrade wgg       ON pcg.pcgrade  = wgg.profitcentregrade
                                 AND pcg.fpsyear  = wgg.fpsyear
JOIN fps.tblwgemployee we         ON wgg.wggrade   = we.workgroupgrade
                                 AND wgg.fpsyear   = we.fpsyear
LEFT JOIN fps.vapphours ah        ON wgg.wggrade   = ah.workgroupgrade
                                 AND wgg.fpsyear   = ah.fpsyear
LEFT JOIN fps.vstaffjobhours sjh  ON we.pactid     = sjh.staffid
                                 AND we.fpsyear    = sjh.fpsyear
GROUP BY pc.conttarget, pcg.profitcentre, pcg.chargerate, pcg.ohr,
         bsum.sumofgenbid, wgg.workgroup, wgg.profitcentregrade, wgg.wggrade,
         ah.sumofplannedhours, we.fpsyear, u.user_id, u.dt2username, u.useremail;
