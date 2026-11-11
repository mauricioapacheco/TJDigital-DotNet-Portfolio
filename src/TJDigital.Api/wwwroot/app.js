'use strict';

const state = { page: 'dashboard', pagina: 1, pageSize: 10, totalPaginas: 1, processoAtual: null, opcoes: null };
const $ = (selector) => document.querySelector(selector);
const escapeHtml = (value) => String(value ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c]);
const dataCurta = (date) => date ? new Intl.DateTimeFormat('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' }).format(new Date(date)) : '—';
const dataCompleta = (date) => date ? new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(date)) : '—';
const classeStatus = (status) => ({ 'Em andamento': 'andamento', 'Concluso': 'concluso', 'Suspenso': 'suspenso', 'Arquivado': 'arquivado', 'Agendada': 'agendada', 'Realizada': 'realizada', 'Cancelada': 'cancelada' })[status] || 'andamento';
const statusTag = (status) => `<span class="status status-${classeStatus(status)}">${escapeHtml(status)}</span>`;
let toastTimeout;

async function api(path, options = {}) {
  let response;
  try {
    response = await fetch(path, { ...options, headers: { ...(options.body ? { 'Content-Type': 'application/json' } : {}), ...(options.headers || {}) } });
  } catch {
    throw new Error('Sem conexão com a aplicação. Verifique se a API está em execução.');
  }
  const result = await response.json().catch(() => ({}));
  if (!response.ok) throw new Error(result.mensagem || result.title || `Erro HTTP ${response.status}`);
  return result;
}

function toast(message, isError = false) {
  const el = $('#toast');
  el.textContent = message;
  el.classList.toggle('error', isError);
  el.classList.add('show');
  clearTimeout(toastTimeout);
  toastTimeout = setTimeout(() => el.classList.remove('show'), 4000);
}
function reportError(error) { toast(error.message || 'Ocorreu um erro inesperado.', true); }
function openDialog(id) { document.getElementById(id).showModal(); }
function closeDialog(id) { document.getElementById(id).close(); }

function navigate(page) {
  if (!['dashboard', 'processos', 'audiencias'].includes(page)) return;
  state.page = page;
  document.querySelectorAll('.page').forEach((el) => { el.hidden = el.id !== `page-${page}`; });
  document.querySelectorAll('.nav-item').forEach((el) => el.classList.toggle('active', el.dataset.page === page));
  $('#breadcrumb').textContent = ({ dashboard: 'Visão geral', processos: 'Processos', audiencias: 'Audiências' })[page];
  $('#sidebar').classList.remove('open');
  location.hash = page;
  if (page === 'dashboard') carregarDashboard();
  if (page === 'processos') carregarProcessos();
  if (page === 'audiencias') carregarAudiencias();
}

async function carregarOpcoes() {
  state.opcoes = await api('/api/opcoes');
  const classes = state.opcoes.classes.map((c) => `<option value="${escapeHtml(c)}">${escapeHtml(c)}</option>`).join('');
  $('#newClasse').innerHTML = `<option value="">Selecione uma classe</option>${classes}`;
  $('#filtroClasse').innerHTML = `<option value="">Todas as classes</option>${classes}`;
  $('#filtroStatus').innerHTML = '<option value="">Todas as situações</option>' + state.opcoes.statusProcesso.map((s) => `<option>${escapeHtml(s)}</option>`).join('');
}

async function carregarDashboard() {
  try {
    const dados = await api('/api/dashboard');
    const cards = [
      ['Total de processos', dados.total, 'Todos os registros', '▤', ''],
      ['Em andamento', dados.emAndamento, 'Tramitação ativa', '↗', 'green'],
      ['Conclusos', dados.conclusos, 'Aguardando decisão', '◷', 'amber'],
      ['Audiências (30 dias)', dados.proximasAudiencias, 'Próximos compromissos', '▦', 'purple']
    ];
    $('#stats').innerHTML = cards.map(([titulo, total, sub, icone, cor]) => `<article class="stat-card"><div class="stat-top"><span>${titulo}</span><span class="stat-icon ${cor}">${icone}</span></div><div class="stat-value">${total}</div><div class="stat-sub">${sub}</div></article>`).join('');
    const max = Math.max(...dados.porClasse.map((x) => x.total), 1);
    $('#classChart').innerHTML = dados.porClasse.map((item) => `<div class="chart-row"><div class="chart-label"><span>${escapeHtml(item.classe)}</span><strong>${item.total}</strong></div><div class="chart-track"><div class="chart-fill" style="width:${(item.total / max * 100).toFixed(1)}%"></div></div></div>`).join('') || '<div class="empty">Nenhum registro cadastrado.</div>';
    $('#recentProcesses').innerHTML = dados.recentes.map((item) => `<div class="recent-row"><div><button type="button" data-detail="${item.id}">${escapeHtml(item.numero)}</button><small>${escapeHtml(item.assunto)}</small></div>${statusTag(item.status)}</div>`).join('') || '<div class="empty">Nenhuma movimentação encontrada.</div>';
  } catch (error) { reportError(error); }
}

async function carregarProcessos() {
  try {
    const query = new URLSearchParams({ page: state.pagina, pageSize: state.pageSize });
    if ($('#busca').value.trim()) query.set('busca', $('#busca').value.trim());
    if ($('#filtroStatus').value) query.set('status', $('#filtroStatus').value);
    if ($('#filtroClasse').value) query.set('classe', $('#filtroClasse').value);
    const dados = await api(`/api/processos?${query}`);
    state.totalPaginas = dados.paginas;
    $('#totalRegistros').textContent = `${dados.total} registro(s)`;
    $('#paginationInfo').textContent = dados.total ? `Página ${dados.pagina} de ${dados.paginas} · ${dados.total} processos` : 'Nenhum registro';
    $('#prevPage').disabled = state.pagina <= 1;
    $('#nextPage').disabled = state.pagina >= dados.paginas;
    $('#processosRows').innerHTML = dados.registros.map((p) => `<tr><td><strong>${escapeHtml(p.numero)}</strong><span class="muted">${escapeHtml(p.classe)}</span></td><td>${escapeHtml(p.assunto)}</td><td>${escapeHtml(p.comarca)}<span class="muted">${escapeHtml(p.vara)}</span></td><td>${dataCurta(p.distribuidoEm)}</td><td>${statusTag(p.status)}</td><td><button class="open-record" type="button" data-detail="${p.id}">Ver detalhes →</button></td></tr>`).join('') || '<tr><td colspan="6" class="empty">Nenhum processo corresponde aos filtros.</td></tr>';
  } catch (error) { reportError(error); }
}

async function abrirDetalhe(id) {
  try {
    const p = await api(`/api/processos/${id}`);
    state.processoAtual = id;
    $('#detailTitle').textContent = p.numero;
    const campo = (rotulo, valor) => `<div class="info-cell"><span>${rotulo}</span><strong>${escapeHtml(valor)}</strong></div>`;
    const partes = p.partes.map((parte) => campo(`Parte ${parte.polo.toLowerCase()}`, parte.nome)).join('');
    const audiencias = p.audiencias.map((a) => `<div class="recent-row"><div><strong>${escapeHtml(a.tipo)} · ${dataCompleta(a.data)}</strong><small>${escapeHtml(a.sala)}</small></div>${statusTag(a.status)}</div>`).join('') || '<p class="empty">Nenhuma audiência registrada.</p>';
    const timeline = p.movimentacoes.map((m) => `<div class="timeline-item"><strong>${escapeHtml(m.descricao)}</strong><p>${dataCompleta(m.data)} · ${escapeHtml(m.responsavel)}</p></div>`).join('') || '<p>Sem movimentações.</p>';
    $('#detailContent').innerHTML = `
      <div>${statusTag(p.status)} <span style="font-size:11px;color:#8291a2;margin-left:8px">Distribuído em ${dataCurta(p.distribuidoEm)}</span></div>
      <div class="detail-grid">${campo('Classe', p.classe)}${campo('Assunto', p.assunto)}${campo('Comarca', p.comarca)}${campo('Vara', p.vara)}${partes}</div>
      <div class="detail-section"><h3>Alterar situação</h3><form id="statusForm" class="inline-form"><label>Situação processual<select name="status">${state.opcoes.statusProcesso.map((x) => `<option ${x === p.status ? 'selected' : ''}>${escapeHtml(x)}</option>`).join('')}</select></label><button type="submit" class="btn btn-primary">Salvar situação</button></form></div>
      <div class="detail-section"><h3>Histórico de movimentações</h3><div class="detail-timeline">${timeline}</div><form id="movementForm" class="inline-form"><label>Nova movimentação<textarea name="descricao" required minlength="5" maxlength="500" rows="2" placeholder="Descreva o andamento fictício"></textarea></label><label>Responsável<input name="responsavel" maxlength="120" required placeholder="Unidade demonstrativa"></label><button class="btn btn-primary" type="submit">Registrar</button></form></div>
      <div class="detail-section"><h3>Audiências vinculadas</h3>${audiencias}</div>`;
    if (!$('#detailDialog').open) openDialog('detailDialog');
  } catch (error) { reportError(error); }
}

async function carregarAudiencias() {
  try {
    const dados = await api('/api/audiencias');
    const status = $('#filtroAudiencia').value;
    const filtradas = status ? dados.filter((a) => a.status === status) : dados;
    $('#audienciasList').innerHTML = filtradas.map((a) => {
      const data = new Date(a.data);
      const mes = new Intl.DateTimeFormat('pt-BR', { month: 'short' }).format(data).replace('.', '');
      return `<div class="agenda-item"><div class="agenda-date"><strong>${data.getDate().toString().padStart(2, '0')}</strong><small>${mes}</small></div><div class="agenda-copy"><strong>${escapeHtml(a.tipo)} · ${escapeHtml(a.numero)}</strong><p>${dataCompleta(a.data)} · ${escapeHtml(a.sala)} · ${escapeHtml(a.comarca)}</p></div><div class="agenda-right">${statusTag(a.status)}<button type="button" data-detail="${a.processoId}">Abrir processo →</button>${a.status === 'Agendada' ? `<button type="button" data-audiencia="${a.id}" data-change-status="Realizada">Marcar realizada</button>` : ''}</div></div>`;
    }).join('') || '<div class="empty">Nenhuma audiência encontrada.</div>';
  } catch (error) { reportError(error); }
}

async function abrirNovaAudiencia() {
  try {
    const dados = await api('/api/processos?page=1&pageSize=50');
    const processos = dados.registros.filter((p) => p.status !== 'Arquivado');
    $('#audProcesso').innerHTML = '<option value="">Selecione o processo</option>' + processos.map((p) => `<option value="${p.id}">${escapeHtml(p.numero)} — ${escapeHtml(p.assunto)}</option>`).join('');
    openDialog('audienciaDialog');
  } catch (error) { reportError(error); }
}

let debounce;
document.addEventListener('click', async (event) => {
  const botao = event.target.closest('button');
  if (!botao) return;
  if (botao.dataset.page) navigate(botao.dataset.page);
  if (botao.dataset.go) navigate(botao.dataset.go);
  if (botao.dataset.close) closeDialog(botao.dataset.close);
  if (botao.dataset.detail) await abrirDetalhe(Number(botao.dataset.detail));
  if (botao.dataset.action === 'novo-processo') openDialog('processDialog');
  if (botao.dataset.action === 'nova-audiencia') await abrirNovaAudiencia();
  if (botao.dataset.audiencia) {
    try {
      await api(`/api/audiencias/${botao.dataset.audiencia}/status`, { method: 'PATCH', body: JSON.stringify({ status: botao.dataset.changeStatus }) });
      toast('Audiência atualizada com sucesso.'); await carregarAudiencias();
    } catch (error) { reportError(error); }
  }
});

document.addEventListener('submit', async (event) => {
  const form = event.target;
  if (!['processForm', 'audienciaForm', 'movementForm', 'statusForm'].includes(form.id)) return;
  event.preventDefault();
  const dados = Object.fromEntries(new FormData(form).entries());
  const enviar = form.querySelector('button[type="submit"]');
  enviar.disabled = true;
  try {
    if (form.id === 'processForm') {
      const criado = await api('/api/processos', { method: 'POST', body: JSON.stringify(dados) });
      form.reset(); closeDialog('processDialog'); toast(`Processo ${criado.numero} cadastrado.`);
      navigate('processos'); await abrirDetalhe(criado.id);
    }
    if (form.id === 'audienciaForm') {
      const data = new Date(dados.data);
      if (Number.isNaN(data.getTime())) throw new Error('Informe uma data válida.');
      await api('/api/audiencias', { method: 'POST', body: JSON.stringify({ processoId: Number(dados.processoId), data: data.toISOString(), tipo: dados.tipo, sala: dados.sala }) });
      form.reset(); closeDialog('audienciaDialog'); toast('Audiência agendada com sucesso.'); await carregarAudiencias();
    }
    if (form.id === 'movementForm') {
      await api(`/api/processos/${state.processoAtual}/movimentacoes`, { method: 'POST', body: JSON.stringify(dados) });
      toast('Movimentação registrada.'); await abrirDetalhe(state.processoAtual);
    }
    if (form.id === 'statusForm') {
      await api(`/api/processos/${state.processoAtual}/status`, { method: 'PUT', body: JSON.stringify({ status: dados.status }) });
      toast('Situação processual atualizada.'); await abrirDetalhe(state.processoAtual);
    }
  } catch (error) { reportError(error); }
  finally { if (enviar.isConnected) enviar.disabled = false; }
});

$('#busca').addEventListener('input', () => { clearTimeout(debounce); debounce = setTimeout(() => { state.pagina = 1; carregarProcessos(); }, 350); });
['filtroStatus', 'filtroClasse'].forEach((id) => document.getElementById(id).addEventListener('change', () => { state.pagina = 1; carregarProcessos(); }));
$('#filtroAudiencia').addEventListener('change', carregarAudiencias);
$('#prevPage').addEventListener('click', () => { state.pagina = Math.max(1, state.pagina - 1); carregarProcessos(); });
$('#nextPage').addEventListener('click', () => { state.pagina = Math.min(state.totalPaginas, state.pagina + 1); carregarProcessos(); });
$('#mobileMenu').addEventListener('click', () => $('#sidebar').classList.toggle('open'));
window.addEventListener('hashchange', () => { const page = location.hash.slice(1); if (page && page !== state.page) navigate(page); });

(async () => {
  try { await carregarOpcoes(); navigate(location.hash.slice(1) || 'dashboard'); }
  catch (error) { reportError(error); }
})();
