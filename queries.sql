-- ============================================================================
-- REQUISITO 1: Ranking dos ramos com maior percentual de sinistros negados nos últimos 6 meses
-- ============================================================================

-- Versão PostgreSQL:
SELECT 
    a.Ramo,
    COUNT(CASE WHEN s.Status = 'Negado' THEN 1 END) * 100.0 / COUNT(*) AS PercentualNegados,
    COUNT(CASE WHEN s.Status = 'Negado' THEN 1 END) AS QtdNegados,
    COUNT(*) AS TotalSinistros
FROM Sinistros s
JOIN Apolices a ON s.ApoliceId = a.Id
WHERE s.DataAbertura >= CURRENT_DATE - INTERVAL '6 months'
GROUP BY a.Ramo
ORDER BY PercentualNegados DESC;

-- Versão SQL Server:
-- SELECT 
--     a.Ramo,
--     CAST(COUNT(CASE WHEN s.Status = 'Negado' THEN 1 END) AS DECIMAL(18,2)) * 100.0 / COUNT(*) AS PercentualNegados,
--     COUNT(CASE WHEN s.Status = 'Negado' THEN 1 END) AS QtdNegados,
--     COUNT(*) AS TotalSinistros
-- FROM Sinistros s
-- JOIN Apolices a ON s.ApoliceId = a.Id
-- WHERE s.DataAbertura >= DATEADD(month, -6, GETDATE())
-- GROUP BY a.Ramo
-- ORDER BY PercentualNegados DESC;


-- ============================================================================
-- REQUISITO 2: Top 10 clientes com maior soma de ValorEstimado em sinistros em análise ou aprovados
-- ============================================================================

-- Versão PostgreSQL:
SELECT 
    c.Id AS ClienteId,
    c.Nome AS ClienteNome,
    SUM(s.ValorEstimado) AS TotalValorEstimado
FROM Sinistros s
JOIN Apolices a ON s.ApoliceId = a.Id
JOIN Clientes c ON a.ClienteId = c.Id
WHERE s.Status IN ('EmAnalise', 'Aprovado')
GROUP BY c.Id, c.Nome
ORDER BY TotalValorEstimado DESC
LIMIT 10;

-- Versão SQL Server:
-- SELECT TOP 10
--     c.Id AS ClienteId,
--     c.Nome AS ClienteNome,
--     SUM(s.ValorEstimado) AS TotalValorEstimado
-- FROM Sinistros s
-- JOIN Apolices a ON s.ApoliceId = a.Id
-- JOIN Clientes c ON a.ClienteId = c.Id
-- WHERE s.Status IN ('EmAnalise', 'Aprovado')
-- GROUP BY c.Id, c.Nome
-- ORDER BY TotalValorEstimado DESC;


-- ============================================================================
-- REQUISITO 3: Tempo médio de resolução (em dias) de sinistros encerrados, agrupado por ramo
-- ============================================================================

-- Versão PostgreSQL:
SELECT 
    a.Ramo,
    AVG(EXTRACT(DAY FROM (h.DataAlteracao - s.DataAbertura))) AS TempoMedioResolucaoDias
FROM Sinistros s
JOIN Apolices a ON s.ApoliceId = a.Id
JOIN HistoricoSinistros h ON s.Id = h.SinistroId
WHERE s.Status = 'Encerrado' AND h.StatusNovo = 'Encerrado'
GROUP BY a.Ramo;

-- Versão SQL Server:
-- SELECT 
--     a.Ramo,
--     AVG(CAST(DATEDIFF(day, s.DataAbertura, h.DataAlteracao) AS DECIMAL(18,2))) AS TempoMedioResolucaoDias
-- FROM Sinistros s
-- JOIN Apolices a ON s.ApoliceId = a.Id
-- JOIN HistoricoSinistros h ON s.Id = h.SinistroId
-- WHERE s.Status = 'Encerrado' AND h.StatusNovo = 'Encerrado'
-- GROUP BY a.Ramo;
