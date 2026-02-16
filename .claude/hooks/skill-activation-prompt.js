#!/usr/bin/env node
/**
 * Skill Auto-Activation Hook
 *
 * Analyzes user prompts and context to automatically suggest relevant skills.
 * This is THE most important hook - prevents CLAUDE.md bloat by loading context conditionally.
 *
 * Usage: Called by Claude Code before processing user input
 * Output: JSON array of skills to activate
 */

const fs = require('fs');
const path = require('path');

// Configuration
const SKILL_RULES_PATH = path.join(__dirname, '../skills/skill-rules.json');
const DEBUG = process.env.SKILL_DEBUG === 'true';

/**
 * Main entry point
 */
function main() {
  try {
    const args = process.argv.slice(2);
    const prompt = args.join(' ') || '';

    if (DEBUG) {
      console.error(`[DEBUG] Analyzing prompt: "${prompt.substring(0, 50)}..."`);
    }

    const skillRules = loadSkillRules();
    const context = gatherContext();
    const activatedSkills = analyzeAndActivateSkills(prompt, context, skillRules);

    // Output JSON for Claude Code to consume
    console.log(JSON.stringify({
      skills: activatedSkills,
      context: {
        working_files: context.workingFiles.length,
        in_git_repo: context.inGitRepo
      }
    }, null, 2));

  } catch (error) {
    console.error(`[ERROR] Skill activation failed: ${error.message}`);
    // Output empty skills array on error (fail gracefully)
    console.log(JSON.stringify({ skills: [] }));
    process.exit(0); // Don't block Claude Code
  }
}

/**
 * Load skill rules configuration
 */
function loadSkillRules() {
  if (!fs.existsSync(SKILL_RULES_PATH)) {
    throw new Error(`Skill rules not found: ${SKILL_RULES_PATH}`);
  }

  const content = fs.readFileSync(SKILL_RULES_PATH, 'utf8');
  return JSON.parse(content);
}

/**
 * Gather context from filesystem and git
 */
function gatherContext() {
  const context = {
    workingFiles: [],
    inGitRepo: false
  };

  // Check if in git repo
  try {
    const { execSync } = require('child_process');
    execSync('git rev-parse --git-dir', { stdio: 'ignore' });
    context.inGitRepo = true;

    // Get modified and staged files
    const output = execSync('git status --short --porcelain', {
      encoding: 'utf8',
      stdio: ['pipe', 'pipe', 'ignore']
    });

    context.workingFiles = output
      .split('\n')
      .filter(line => line.trim())
      .map(line => line.substring(3).trim())
      .filter(file => file.length > 0);

  } catch (error) {
    // Not in git repo or git not available - that's OK
    if (DEBUG) {
      console.error('[DEBUG] Git context unavailable:', error.message);
    }
  }

  return context;
}

/**
 * Analyze prompt and context to activate matching skills
 */
function analyzeAndActivateSkills(prompt, context, skillRules) {
  const activated = [];

  for (const skillRule of skillRules.skills) {
    const confidence = calculateConfidence(prompt, context, skillRule);

    if (confidence >= skillRule.triggers.confidence_threshold) {
      activated.push({
        name: skillRule.name,
        confidence: parseFloat(confidence.toFixed(2)),
        reason: getActivationReason(prompt, context, skillRule)
      });
    }
  }

  // Sort by confidence (highest first)
  return activated.sort((a, b) => b.confidence - a.confidence);
}

/**
 * Calculate confidence score for skill activation
 */
function calculateConfidence(prompt, context, skillRule) {
  const scores = [];

  // Keyword matching score
  if (skillRule.triggers.keywords && skillRule.triggers.keywords.length > 0) {
    const keywordScore = calculateKeywordScore(prompt, skillRule.triggers.keywords);
    scores.push({ score: keywordScore, weight: 0.6 });
  }

  // Path matching score
  if (skillRule.triggers.paths && skillRule.triggers.paths.length > 0) {
    const pathScore = calculatePathScore(context.workingFiles, skillRule.triggers.paths);
    scores.push({ score: pathScore, weight: 0.4 });
  }

  // Calculate weighted average
  if (scores.length === 0) {
    return 0;
  }

  const totalWeight = scores.reduce((sum, s) => sum + s.weight, 0);
  const weightedSum = scores.reduce((sum, s) => sum + (s.score * s.weight), 0);

  return weightedSum / totalWeight;
}

/**
 * Calculate keyword match score with synonym support
 */
function calculateKeywordScore(prompt, keywords) {
  const promptLower = prompt.toLowerCase();

  // Keyword synonyms/variations
  const keywordSynonyms = {
    'adr': ['architecture decision', 'decision record', 'architectural decision'],
    'validation': ['validate', 'check', 'verify', 'lint'],
    'documentation': ['docs', 'readme', 'document'],
    'consistency': ['sync', 'match', 'align', 'consistent']
  };

  let matches = 0;

  for (const keyword of keywords) {
    const keywordLower = keyword.toLowerCase();

    // Direct match
    if (promptLower.includes(keywordLower)) {
      matches++;
      continue;
    }

    // Synonym match
    const synonyms = keywordSynonyms[keywordLower] || [];
    if (synonyms.some(syn => promptLower.includes(syn))) {
      matches++;
    }
  }

  return Math.min(matches / keywords.length, 1.0);
}

/**
 * Calculate path match score
 */
function calculatePathScore(workingFiles, pathPatterns) {
  if (workingFiles.length === 0) {
    return 0;
  }

  const matchedFiles = workingFiles.filter(file =>
    pathPatterns.some(pattern => matchGlobPattern(file, pattern))
  );

  // Higher score if more files match
  return Math.min(matchedFiles.length / Math.max(workingFiles.length, 1), 1.0);
}

/**
 * Simple glob pattern matching
 */
function matchGlobPattern(filepath, pattern) {
  // Convert glob pattern to regex
  const regexPattern = pattern
    .replace(/\*\*/g, '§GLOBSTAR§')  // Placeholder for **
    .replace(/\*/g, '[^/]*')          // * matches anything except /
    .replace(/§GLOBSTAR§/g, '.*')     // ** matches anything including /
    .replace(/\?/g, '.')              // ? matches single char
    .replace(/\./g, '\\.');           // Escape dots

  const regex = new RegExp('^' + regexPattern + '$');
  return regex.test(filepath);
}

/**
 * Get human-readable activation reason
 */
function getActivationReason(prompt, context, skillRule) {
  const reasons = [];

  // Keyword matches
  const matchedKeywords = (skillRule.triggers.keywords || []).filter(kw =>
    prompt.toLowerCase().includes(kw.toLowerCase())
  );

  if (matchedKeywords.length > 0) {
    reasons.push(`keywords: ${matchedKeywords.join(', ')}`);
  }

  // Path matches
  const matchedPaths = context.workingFiles.filter(file =>
    (skillRule.triggers.paths || []).some(pattern => matchGlobPattern(file, pattern))
  );

  if (matchedPaths.length > 0) {
    reasons.push(`files: ${matchedPaths.slice(0, 3).join(', ')}${matchedPaths.length > 3 ? '...' : ''}`);
  }

  return reasons.join('; ') || 'pattern match';
}

// Execute main
if (require.main === module) {
  main();
}

module.exports = { analyzeAndActivateSkills, calculateConfidence };
